import { AuthForm } from "../components/AuthForm";
import { GraduationCap, CheckCircle2 } from "lucide-react";
import Education from "../../assets/Education.svg";
import schoolbus from "../../assets/schoolbus.svg";
const LoginPage: React.FC = () => {
  return (
    <div className="min-h-screen w-full grid lg:grid-cols-2">
      {/* Left Column: Form */}
      <div className="flex min-h-screen flex-col justify-center items-center px-4 py-12 sm:px-6 lg:px-8">
        <div className="w-full max-w-md">
          {/* School Bus */}
          <div className="flex justify-center mb-2">
            <img
              src={schoolbus}
              alt="School bus"
              className="w-65 h-auto object-contain"
            />
          </div>

          {/* Portal Branding */}
          <div className="mb-8 flex items-center justify-center gap-3">
            <div className="p-2 rounded-xl bg-green-100 text-green-700">
              <GraduationCap className="h-6 w-6" />
            </div>

            <div>
              <h1 className="text-xl font-bold tracking-tight">
                Academix Portal
              </h1>

              <p className="text-xs text-muted-foreground">
                Student & Staff Portal
              </p>
            </div>
          </div>

          {/* Login Form */}
          <AuthForm />
        </div>
      </div>

      {/* Right Column: Hero Visual (Simple, Clean Vibe) */}
      <div className="hidden lg:flex flex-col justify-between p-12 bg-linear-to-br from-blue-200 via-blue-400 to-slate-400">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-lg bg-white/10 backdrop-blur flex items-center justify-center font-bold">
            A
          </div>
          <span className="font-semibold tracking-wide">Academix System</span>
        </div>
        <div className="flex justify-center">
          <img
            alt="Education"
            src={Education}
            className="w-65 h-auto object-fill "
          ></img>
        </div>

        <div className="max-w-md space-y-4">
          <h2 className="text-3xl font-bold tracking-tight leading-tight">
            Streamlined student submissions and class attendance.
          </h2>
          <p className="text-primary-foreground/80 text-sm">
            Access your submissions, real-time attendance reports, grades, and
            class schedules all in one unified dashboard.
          </p>

          <div className="pt-4 space-y-2 text-sm text-primary-foreground/90">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4  font-extrabold" />
              <span>Real-time attendance & grade tracking</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 font-extrabold" />
              <span>Direct assignments & submission management</span>
            </div>
          </div>
        </div>

        <p className="text-xs text-primary-foreground/60">
          © {new Date().getFullYear()} Academix. All rights reserved.
        </p>
      </div>
    </div>
  );
};

export default LoginPage;
