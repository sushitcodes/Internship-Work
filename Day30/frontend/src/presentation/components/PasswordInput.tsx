import { useState } from "react";
import { UseFormRegisterReturn, FieldError } from "react-hook-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Eye, EyeOff } from "lucide-react";
import { cn } from "@/lib/utils";

interface PasswordInputProps {
  label: string;
  id?: string;
  placeholder?: string;
  registration: UseFormRegisterReturn;
  error?: FieldError;
  className?: string;
}

export function PasswordInput({
  label,
  id,
  placeholder,
  registration,
  error,
  className,
}: PasswordInputProps) {
  const [visible, setVisible] = useState(false);
  const inputId = id || registration.name;

  return (
    <div className={cn("space-y-2", className)}>
      <Label htmlFor={inputId}>{label}</Label>
      <div className="relative">
        <Input
          id={inputId}
          type={visible ? "text" : "password"}
          placeholder={placeholder}
          {...registration}
          className={cn(
            "pr-10",
            error ? "border-red-500 focus-visible:ring-red-500" : "",
          )}
        />
        <button
          type="button"
          tabIndex={-1}
          onClick={() => setVisible((v) => !v)}
          className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
        >
          {visible ? (
            <EyeOff className="h-4 w-4" />
          ) : (
            <Eye className="h-4 w-4" />
          )}
        </button>
      </div>
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}
