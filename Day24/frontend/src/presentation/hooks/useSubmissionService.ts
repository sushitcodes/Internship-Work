// presentation/hooks/useSubmissionService.ts
// The dependency-injection point. Components ask for "something that
// satisfies ISubmissionService" — today that's axios, but nothing in
// Presentation ever names axios or the concrete class directly.
import { submissionService } from "../../infrastructure/services/SubmissionService";
import { ISubmissionService } from "../../application/interfaces/ISubmissionService";

export const useSubmissionService = (): ISubmissionService => {
  return submissionService;
};
