import { useState } from "react";
import { useGetClassRoomsQuery } from "../../infrastructure/api/classRoomApi";
import {
  GradeRosterEntry,
  useGetSubjectsByClassQuery,
  useGetGradeRosterQuery,
  useSubmitGradesMutation,
} from "../../infrastructure/api/gradeApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { toast } from "sonner";
import { IdSelect } from "../components/IdSelect";
import { PageHeader } from "../components/PageHeader";
import { extractErrorMessage } from "@/lib/apiError";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface DraftGrade {
  marksObtained: string; // string while typing, parsed to number on save
  maxMarks: string;
  remarks: string;
}

// Explicit type for the payload entries, so the filter predicate
// can narrow correctly without the circular `typeof e`.
interface SubmitGradeEntry {
  enrollmentId: string;
  marksObtained: number;
  maxMarks: number;
  remarks: string | undefined;
}

interface GradeRosterEditorProps {
  roster: GradeRosterEntry[];
  subjectId: string;
}

// ---------------------------------------------------------------------------
// Validation
// ---------------------------------------------------------------------------

// Returns an error message for one row, or null when the row is fine.
// A blank "Marks" field means "not graded yet", so that is allowed.
function validateRow(draft?: DraftGrade): string | null {
  if (!draft || draft.marksObtained.trim() === "") return null;

  const marks = Number(draft.marksObtained);
  const max = draft.maxMarks.trim() === "" ? 100 : Number(draft.maxMarks);

  if (Number.isNaN(max) || max <= 0) return "“Out of” must be more than 0.";
  if (Number.isNaN(marks)) return "Enter a valid number.";
  if (marks < 0) return "Marks cannot be negative.";
  if (marks > max) return `Marks cannot be more than ${max}.`;
  return null;
}

// ---------------------------------------------------------------------------
// Parent: EnterGradesPage
// ---------------------------------------------------------------------------

function EnterGradesPage() {
  const { data: classRooms } = useGetClassRoomsQuery();
  const [classRoomId, setClassRoomId] = useState<string>("");
  const [subjectId, setSubjectId] = useState<string>("");

  const { data: subjects } = useGetSubjectsByClassQuery(classRoomId, {
    skip: !classRoomId,
  });

  const { data: roster, isLoading: isLoadingRoster } = useGetGradeRosterQuery(
    subjectId,
    { skip: !subjectId },
  );

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <PageHeader
        title="Grade Entry"
        description="Select a class and subject to record or update term marks."
      />
      <Card className="border-border/70 shadow-xs">
        <CardHeader className="border-b bg-muted/20 pb-3">
          <CardTitle className="text-base font-semibold">
            Select Class & Subject
          </CardTitle>
        </CardHeader>

        <CardContent className="flex gap-3 items-end">
          <div>
            <label className="text-sm font-medium mb-2 block">Class</label>
            <IdSelect
              options={classRooms?.map((c) => ({ id: c.id, label: c.name }))}
              value={classRoomId}
              onValueChange={(v) => {
                setClassRoomId(v);
                setSubjectId(""); // changing class invalidates the picked subject
              }}
              placeholder="Select a class"
              className="w-48"
            />
          </div>

          <div>
            <label className="text-sm font-medium mb-2 block">Subject</label>
            <IdSelect
              options={subjects?.map((s) => ({ id: s.id, label: s.name }))}
              value={subjectId}
              onValueChange={setSubjectId}
              placeholder="Select a subject"
              className="w-48"
            />
          </div>
        </CardContent>
      </Card>

      {subjectId && isLoadingRoster && (
        <p className="text-sm text-muted-foreground">Loading roster...</p>
      )}

      {/* empty roster state, so the user sees why nothing rendered */}
      {subjectId && roster && roster.length === 0 && (
        <p className="text-sm text-muted-foreground">
          No students are enrolled in this subject's classroom.
        </p>
      )}

      {subjectId && roster && roster.length > 0 && (
        /*
          key={subjectId} is the whole trick:
            - Same subject: React keeps the same editor, so drafts survive refetches.
            - Different subject: React unmounts and remounts, so drafts reset from the fresh roster.
        */
        <GradeRosterEditor
          key={subjectId}
          roster={roster}
          subjectId={subjectId}
        />
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Child: GradeRosterEditor owns the drafts
// ---------------------------------------------------------------------------

function GradeRosterEditor({ roster, subjectId }: GradeRosterEditorProps) {
  /*
    Lazy initializer runs ONCE when this component mounts.
    Because of key={subjectId}, that "once" is per subject:
      - switching subjects remounts, so drafts are fresh
      - refetches of the SAME subject do NOT remount, so drafts survive
  */
  const [drafts, setDrafts] = useState<Record<string, DraftGrade>>(() => {
    const initial: Record<string, DraftGrade> = {};
    roster.forEach((r) => {
      initial[r.enrollmentId] = {
        marksObtained: r.marksObtained?.toString() ?? "",
        maxMarks: r.maxMarks.toString(),
        remarks: r.remarks ?? "",
      };
    });
    return initial;
  });

  const [submitGrades, { isLoading: isSaving }] = useSubmitGradesMutation();

  // True when at least one row has a problem. Used to disable Save.
  const hasErrors = roster.some(
    (r) => validateRow(drafts[r.enrollmentId]) !== null,
  );

  // Single helper for all three inputs.
  const updateDraft = (enrollmentId: string, patch: Partial<DraftGrade>) => {
    setDrafts((prev) => {
      const existing = prev[enrollmentId];
      return {
        ...prev,
        [enrollmentId]: {
          marksObtained: existing?.marksObtained ?? "",
          maxMarks: existing?.maxMarks ?? "100",
          remarks: existing?.remarks ?? "",
          ...patch,
        },
      };
    });
  };

  const handleSubmit = async () => {
    if (!subjectId) return;

    // Same rule as the disabled button, in case it is triggered another way.
    if (hasErrors) {
      toast.error("Fix the marks highlighted in red before saving.");
      return;
    }

    // Only send rows where marks were actually entered. A student left
    // blank stays ungraded instead of getting silently zeroed out.
    const entries: SubmitGradeEntry[] = roster
      .map((r): SubmitGradeEntry | null => {
        const draft = drafts[r.enrollmentId];
        return draft && draft.marksObtained !== ""
          ? {
              enrollmentId: r.enrollmentId,
              marksObtained: Number(draft.marksObtained),
              maxMarks: Number(draft.maxMarks) || 100,
              remarks: draft.remarks || undefined,
            }
          : null;
      })
      .filter((e): e is SubmitGradeEntry => e !== null);

    if (entries.length === 0) {
      toast.error("Enter at least one grade before saving.");
      return;
    }

    try {
      await submitGrades({ subjectId, entries }).unwrap();
      toast.success("Grades saved.");
    } catch (err) {
      console.error("Failed to save grades:", err);
      // Shows the server's own message when there is one.
      toast.error(
        extractErrorMessage(err, "Could not save grades. Please try again."),
      );
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Students</CardTitle>
      </CardHeader>

      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Student</TableHead>
              <TableHead>Marks</TableHead>
              <TableHead>Out of</TableHead>
              <TableHead>Remarks</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {roster.map((r) => {
              const draft = drafts[r.enrollmentId];
              const max = Number(draft?.maxMarks) || 100;
              const error = validateRow(draft);

              return (
                <TableRow key={r.enrollmentId}>
                  <TableCell>{r.studentName}</TableCell>

                  <TableCell>
                    <Input
                      type="number"
                      className={`w-20 ${
                        error ? "border-red-500 focus-visible:ring-red-500" : ""
                      }`}
                      aria-invalid={!!error}
                      disabled={isSaving}
                      min={0}
                      max={max}
                      value={draft?.marksObtained ?? ""}
                      onChange={(e) =>
                        updateDraft(r.enrollmentId, {
                          marksObtained: e.target.value,
                        })
                      }
                    />
                    {error && (
                      <p className="mt-1 text-xs text-red-500">{error}</p>
                    )}
                  </TableCell>

                  <TableCell>
                    <Input
                      type="number"
                      className="w-20"
                      disabled={isSaving}
                      min={1}
                      value={draft?.maxMarks ?? "100"}
                      onChange={(e) =>
                        updateDraft(r.enrollmentId, {
                          maxMarks: e.target.value,
                        })
                      }
                    />
                  </TableCell>

                  <TableCell>
                    <Input
                      className="w-40"
                      disabled={isSaving}
                      value={draft?.remarks ?? ""}
                      onChange={(e) =>
                        updateDraft(r.enrollmentId, {
                          remarks: e.target.value,
                        })
                      }
                    />
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>

        <Button
          onClick={handleSubmit}
          disabled={isSaving || hasErrors}
          className="mt-4 w-full"
        >
          {isSaving ? "Saving..." : "Save Grades"}
        </Button>
      </CardContent>
    </Card>
  );
}

export default EnterGradesPage;
