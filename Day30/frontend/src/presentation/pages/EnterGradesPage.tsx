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

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface DraftGrade {
  marksObtained: string; // string while typing — parsed to number on save
  maxMarks: string;
  remarks: string;
}

// — explicit type for the payload entries, so the filter predicate
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
// Parent — EnterGradesPage
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
  // this does that when i go back the roster is called and previous draft value is not saved
  // // get deleted so i nee this new react method with key
  //   const [drafts, setDrafts] = useState<Record<string, DraftGrade>>({});
  //   useEffect(() => {
  //     if (roster) {
  //       const fromServer: Record<string, DraftGrade> = {};
  //       roster.forEach((r) => {
  //         fromServer[r.enrollmentId] = {
  //           marksObtained: r.marksObtained?.toString() ?? "",
  //           maxMarks: r.maxMarks.toString(),
  //           remarks: r.remarks ?? "",
  //         };
  //       });
  //       setDrafts(fromServer);
  //     }
  //   }, [roster]);

  return (
    <div className="max-w-3xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Enter Grades</CardTitle>
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
            - Same subject → React keeps the same editor → drafts preserved across refetches.
            - Different subject → React unmounts + remounts → drafts reset from fresh roster.
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
// Child — GradeRosterEditor owns the drafts
// ---------------------------------------------------------------------------

function GradeRosterEditor({ roster, subjectId }: GradeRosterEditorProps) {
  /*
    Lazy initializer runs ONCE when this component mounts.
    Because of key={subjectId}, that "once" is per-subject:
      - switching subjects remounts → fresh drafts
      - refetches of the SAME subject do NOT remount → drafts survive
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

  // single helper for all three inputs.
  // Spread `prev[enrollmentId]` first (to keep any existing values), then
  // the caller's patch on top. Defaults provide a valid shape for new rows.
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

    // Only send rows where marks were actually entered — a student left
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
      toast.error("Could not save grades. Please try again.");
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

              return (
                <TableRow key={r.enrollmentId}>
                  <TableCell>{r.studentName}</TableCell>

                  <TableCell>
                    <Input
                      type="number"
                      className="w-20"
                      // FIX #7 — disable while saving
                      disabled={isSaving}
                      // FIX #4 — client-side cap (optional but helpful)
                      min={0}
                      max={max}
                      value={draft?.marksObtained ?? ""}
                      onChange={(e) =>
                        updateDraft(r.enrollmentId, {
                          marksObtained: e.target.value,
                        })
                      }
                    />
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
          disabled={isSaving}
          className="mt-4 w-full"
        >
          {isSaving ? "Saving..." : "Save Grades"}
        </Button>
      </CardContent>
    </Card>
  );
}

export default EnterGradesPage;
