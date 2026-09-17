import type { AppDispatch } from "../store";
import { submissionApi } from "./submissionApi";
import { userApi } from "./userApi";
import { attendanceApi } from "./attendanceApi";
import { classRoomApi } from "./classRoomApi";
import { dashboardApi } from "./dashboardApi";
import { enrollmentApi } from "./enrollmentApi";
import { gradeApi } from "./gradeApi";
// Single place to reset every RTK Query cache. Add new api slices here

export function clearAllApiCaches(dispatch: AppDispatch) {
  dispatch(submissionApi.util.resetApiState());
  dispatch(userApi.util.resetApiState());
  dispatch(attendanceApi.util.resetApiState());
  dispatch(classRoomApi.util.resetApiState());
  dispatch(dashboardApi.util.resetApiState());
  dispatch(enrollmentApi.util.resetApiState());
  dispatch(gradeApi.util.resetApiState());
}
