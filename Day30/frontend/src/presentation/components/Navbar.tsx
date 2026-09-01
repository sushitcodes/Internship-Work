import { Link, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useLogoutUserMutation } from "../../infrastructure/api/authApi";

const Navbar: React.FC = () => {
  const email = useAppSelector((state) => state.auth.email);
  const roles = useAppSelector((state) => state.auth.roles);
  const [logoutUser] = useLogoutUserMutation();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logoutUser();
    navigate("/login");
  };
  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      {" "}
      <div className="max-w-3xl mx-auto flex items-center justify-between py-3 px-4">
        <Link to="/" className="font-semibold text-lg">
          Student Submission
        </Link>
        {roles.includes("Admin") && (
          <Link to="/classes">
            <Button variant="ghost" size="sm">
              Classes
            </Button>
          </Link>
        )}
        <div className="flex items-center gap-3">
          {email ? (
            <>
              <span className="text-sm text-muted-foreground">{email}</span>
              <Button variant="outline" size="sm" onClick={handleLogout}>
                Log Out
              </Button>
            </>
          ) : (
            <>
              <Link to="/login">
                <Button variant="outline" size="sm">
                  Log In
                </Button>
              </Link>
              <Link to="/register">
                <Button size="sm">Register</Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
};

export default Navbar;
