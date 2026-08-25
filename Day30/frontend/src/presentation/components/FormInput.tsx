import { UseFormRegisterReturn, FieldError } from "react-hook-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

interface FormInputProps {
  label: string;
  id?: string;
  type?: string;
  placeholder?: string;
  registration: UseFormRegisterReturn;
  error?: FieldError;
  className?: string;
}

export function FormInput({
  label,
  id,
  type = "text",
  placeholder,
  registration,
  error,
  className,
}: FormInputProps) {
  const inputId = id || registration.name;

  return (
    <div className={cn("space-y-2", className)}>
      <Label htmlFor={inputId}>{label}</Label>
      <Input
        id={inputId}
        type={type}
        placeholder={placeholder}
        {...registration}
        className={error ? "border-red-500 focus-visible:ring-red-500" : ""}
      />
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}
