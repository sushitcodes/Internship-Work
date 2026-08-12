import type {
  FormState,
  FormActionTypes,
  FormData,
} from "../../types/formTypes";

import {
  ADD_SUBMISSION,
  CLEAR_SUBMISSIONS,
  DELETE_SUBMISSION, // ← Must import this!
} from "../../types/formTypes";

const initialState: FormState = {
  submissions: [],
};

const formReducer = (
  state = initialState,
  action: FormActionTypes,
): FormState => {
  switch (action.type) {
    case ADD_SUBMISSION: {
      const newSubmission: FormData = {
        ...action.payload,
        id: Date.now().toString(),
        submittedAt: new Date().toISOString(),
      };

      return {
        ...state,
        submissions: [...state.submissions, newSubmission],
      };
    }

    case CLEAR_SUBMISSIONS: {
      return {
        ...state,
        submissions: [],
      };
    }

    case DELETE_SUBMISSION: {
      return {
        ...state,
        submissions: state.submissions.filter(
          (submission) => submission.id !== action.payload,
        ),
      };
    }

    default:
      return state;
  }
};

export default formReducer;
