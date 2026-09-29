import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getInitials } from "@/application/utils/getInitials";
import { resolveFileUrl } from "@/lib/resolveFileUrl";
import { cn } from "@/lib/utils";

interface UserAvatarProps {
  name: string;
  src?: string | null;
  className?: string;
  fallbackClassName?: string;
}

export function UserAvatar({
  name,
  src,
  className,
  fallbackClassName,
}: UserAvatarProps) {
  return (
    <Avatar className={cn("h-8 w-8", className)}>
      <AvatarImage src={resolveFileUrl(src)} alt={name} />
      <AvatarFallback className={cn("text-xs", fallbackClassName)}>
        {getInitials(name)}
      </AvatarFallback>
    </Avatar>
  );
}
