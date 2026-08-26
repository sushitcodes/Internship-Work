import { Link, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  useAppSelector,
  useAppDispatch,
} from "../../infrastructure/store/hooks";
import { logout } from "../../infrastructure/store/authSlice";
const Navbar: React.FC = () => {
  const email = useAppSelector((state) => state.auth.email);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  };
  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      {" "}
      <div className="max-w-3xl mx-auto flex items-center justify-between py-3 px-4">
        <Link to="/" className="font-semibold text-lg">
          Submissions
        </Link>
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
