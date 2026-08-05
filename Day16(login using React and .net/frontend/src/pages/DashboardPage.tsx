import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";

function DashboardPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="auth-page">
      <div className="auth-form">
        <h1>Dashboard</h1>
        <p>You are logged in.</p>
        <button onClick={handleLogout}>Logout</button>
      </div>
    </div>
  );
}

export default DashboardPage;
