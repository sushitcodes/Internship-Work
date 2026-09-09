import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";

interface IdSelectOption {
  id: string;
  label: string;
}

interface IdSelectProps {
  options: IdSelectOption[] | undefined;
  value: string;
  onValueChange: (value: string) => void;
  placeholder: string;
  className?: string;
}
// Wraps shadcn's Select so every id-based dropdown in the app gets correct
// label display for free — the function-child lookup lives HERE, once,
// instead of being copy-pasted into every page that needs a picker.
export function IdSelect({
  options,
  value,
  onValueChange,
  placeholder,
  className,
}: IdSelectProps) {
  return (
    <Select onValueChange={(v) => onValueChange(v ?? "")} value={value}>
      <SelectTrigger className={className}>
        <SelectValue placeholder={placeholder}>
          {(val: string) => options?.find((o) => o.id === val)?.label}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        {options?.map((o) => (
          <SelectItem key={o.id} value={o.id}>
            {o.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
