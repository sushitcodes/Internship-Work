import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useGetMeQuery } from "../../infrastructure/api/authApi";

const ProtectedRoute: React.FC = () => {
  const { isLoading } = useGetMeQuery();
  const email = useAppSelector((state) => state.auth.email);

  if (isLoading) return null; // don't decide yet — /me hasn't resolved
  return email ? <Outlet /> : <Navigate to="/login" replace />;
};
export default ProtectedRoute;
