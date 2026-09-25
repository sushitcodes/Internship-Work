import React, { useState } from "react";
import { useGetClassRoomsQuery } from "../../infrastructure/api/classRoomApi";
import {
  downloadImportTemplate,
  useBulkImportStudentsMutation,
  type BulkImportResult,
} from "../../infrastructure/api/bulkImportApi";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { IdSelect } from "./IdSelect";
import {
  UploadCloud,
  FileSpreadsheet,
  Download,
  AlertCircle,
} from "lucide-react";
import { toast } from "sonner";

interface BulkImportModalProps {
  onSuccess: () => void;
}

export const BulkImportModal: React.FC<BulkImportModalProps> = ({
  onSuccess,
}) => {
  const [open, setOpen] = useState(false);
  const [classRoomId, setClassRoomId] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [result, setResult] = useState<BulkImportResult | null>(null);

  const { data: classRooms } = useGetClassRoomsQuery();
  const [bulkImport, { isLoading: isUploading }] =
    useBulkImportStudentsMutation();

  const handleDownloadTemplate = async () => {
    try {
      await downloadImportTemplate();
      toast.success("Sample template downloaded.");
    } catch {
      toast.error("Could not download template.");
    }
  };

  const handleUpload = async () => {
    if (!selectedFile || !classRoomId) return;
    setResult(null);
    try {
      const res = await bulkImport({
        file: selectedFile,
        classRoomId,
      }).unwrap();
      setResult(res);

      if (res.success) {
        toast.success(`Successfully enrolled ${res.importedCount} students!`);
        onSuccess();
        setTimeout(() => {
          setOpen(false);
          setResult(null);
          setSelectedFile(null);
        }, 1500);
      } else {
        toast.error("Validation failed. Please check the error list.");
      }
    } catch (err: any) {
      const message =
        err?.data?.message ?? err?.data ?? err?.message ?? "Import failed.";
      toast.error(typeof message === "string" ? message : "Import failed.");
    }
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button variant="outline" size="sm" className="gap-2 shadow-xs">
            <UploadCloud className="h-4 w-4" />
            Bulk Import
          </Button>
        }
      />
      <PopoverContent align="end" className="w-95 sm:w-110 p-5 shadow-xl">
        <div className="space-y-4">
          <div className="flex items-center justify-between border-b pb-3">
            <div>
              <h3 className="font-semibold text-base text-foreground">
                Bulk Student Import
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5">
                Upload an Excel file (.xlsx) to enroll students in bulk
              </p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={handleDownloadTemplate}
              className="text-xs gap-1.5 text-primary hover:text-primary"
              title="Download Sample Template"
            >
              <Download className="h-3.5 w-3.5" />
              Template
            </Button>
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-muted-foreground uppercase">
              Target Classroom
            </label>
            <IdSelect
              options={classRooms?.map((c) => ({ id: c.id, label: c.name }))}
              value={classRoomId}
              onValueChange={setClassRoomId}
              placeholder="Select destination class"
              className="w-full"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-muted-foreground uppercase">
              Excel Spreadsheet (.xlsx)
            </label>
            <div className="border-2 border-dashed border-border/80 rounded-xl p-4 text-center hover:bg-muted/30 transition-colors">
              <input
                type="file"
                accept=".xlsx"
                id="bulk-file"
                className="hidden"
                onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
              />
              <label htmlFor="bulk-file" className="cursor-pointer block">
                <FileSpreadsheet className="h-8 w-8 text-primary mx-auto mb-2 opacity-80" />
                <span className="text-sm font-medium text-foreground block">
                  {selectedFile
                    ? selectedFile.name
                    : "Click to select Excel file"}
                </span>
                <span className="text-xs text-muted-foreground mt-0.5 block">
                  {selectedFile
                    ? `${Math.round(selectedFile.size / 1024)} KB`
                    : "Supports .xlsx spreadsheets"}
                </span>
              </label>
            </div>
          </div>

          {result && !result.success && (
            <div className="bg-destructive/10 border border-destructive/20 rounded-xl p-3 text-xs max-h-36 overflow-y-auto space-y-1">
              <div className="font-semibold text-destructive flex items-center gap-1.5 mb-1">
                <AlertCircle className="h-4 w-4" />
                {result.errors.length} issue(s) prevented import:
              </div>
              {result.errors.map((err, i) => (
                <p key={i} className="text-destructive/90">
                  • {err}
                </p>
              ))}
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2 border-t">
            <Button variant="ghost" size="sm" onClick={() => setOpen(false)}>
              Cancel
            </Button>
            <Button
              size="sm"
              onClick={handleUpload}
              disabled={isUploading || !selectedFile || !classRoomId}
              className="gap-2 shadow-xs"
            >
              <UploadCloud className="h-4 w-4" />
              {isUploading ? "Importing..." : "Start Import"}
            </Button>
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
};
