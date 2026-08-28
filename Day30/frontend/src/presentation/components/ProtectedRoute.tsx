import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useGetMeQuery } from "../../infrastructure/api/authApi";

interface ProtectedRouteProps {
  allowedRoles?: string[]; // omit = any logged-in user is fine
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { isLoading } = useGetMeQuery();
  const email = useAppSelector((state) => state.auth.email);
  const role = useAppSelector((state) => state.auth.role);

  if (isLoading) return null;
  if (!email) return <Navigate to="/login" replace />;
  if (allowedRoles && !allowedRoles.includes(role ?? "")) {
    return <Navigate to="/" replace />; // logged in, just wrong role
  }

  return <Outlet />;
};

export default ProtectedRoute;
