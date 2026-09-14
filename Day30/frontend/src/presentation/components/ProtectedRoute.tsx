import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useGetMeQuery } from "../../infrastructure/api/authApi";
import { Paths } from "../../routes/paths";
interface ProtectedRouteProps {
  allowedRoles?: string[]; // omit = any logged-in user is fine
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { isLoading } = useGetMeQuery();
  const email = useAppSelector((state) => state.auth.email);
  const roles = useAppSelector((state) => state.auth.roles);

  if (isLoading) return null;
  if (!email) return <Navigate to={Paths.login} replace />;
  if (allowedRoles && !roles.some((r) => allowedRoles.includes(r))) {
    return <Navigate to={Paths.dashboard} replace />;
  }
  return <Outlet />;
};
export default ProtectedRoute;
