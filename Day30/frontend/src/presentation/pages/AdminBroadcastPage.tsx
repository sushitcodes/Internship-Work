import { useForm, SubmitHandler } from "react-hook-form";
import { useBroadcastNotificationMutation } from "../../infrastructure/api/notificationApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { FormInput } from "../components/FormInput";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";

interface BroadcastFormValues {
  title: string;
  body: string;
  link: string;
  scope: "AllTeachers" | "AllStudents" | "Everyone";
}

export default function AdminBroadcastPage() {
  const [broadcast, { isLoading }] = useBroadcastNotificationMutation();
  const { register, handleSubmit, reset, setValue, watch } =
    useForm<BroadcastFormValues>({
      defaultValues: { scope: "Everyone" },
    });
  const scope = watch("scope");

  const onSubmit: SubmitHandler<BroadcastFormValues> = async (data) => {
    try {
      await broadcast({
        title: data.title,
        body: data.body,
        link: data.link || undefined,
        scope: data.scope,
      }).unwrap();
      toast.success("Notification sent.");
      reset({ scope: "Everyone", title: "", body: "", link: "" });
    } catch {
      toast.error("Could not send notification.");
    }
  };

  return (
    <Card className="max-w-xl mx-auto mt-10">
      <CardHeader>
        <CardTitle>Send Notification</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <FormInput
            label="Title"
            registration={register("title", { required: true, maxLength: 200 })}
          />
          <FormInput
            label="Body"
            registration={register("body", { required: true, maxLength: 1000 })}
          />
          <FormInput label="Link (optional)" registration={register("link")} />
          <div className="space-y-2">
            <Label>Recipients</Label>
            <Select
              value={scope}
              onValueChange={(v) =>
                setValue(
                  "scope",
                  (v ?? "Everyone") as BroadcastFormValues["scope"],
                )
              }
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="AllTeachers">All Teachers</SelectItem>
                <SelectItem value="AllStudents">All Students</SelectItem>
                <SelectItem value="Everyone">Everyone</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Sending..." : "Send"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
