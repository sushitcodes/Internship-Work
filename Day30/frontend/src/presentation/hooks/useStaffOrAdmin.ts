import { canEdit } from "@/presentation/config/authUiConfig";
import { useAppSelector } from "@/infrastructure/store/hooks";

export function useStaffOrAdmin(): boolean {
  const roles = useAppSelector((state) => state.auth.roles);
  return canEdit(roles);
}
