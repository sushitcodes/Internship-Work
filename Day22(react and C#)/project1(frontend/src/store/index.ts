import { createStore, combineReducers } from "redux";
import formReducer from "./reducers/formReducer";
import type { FormState } from "../types/formTypes";

export interface RootState {
  form: FormState;
}

const rootReducer = combineReducers({
  form: formReducer,
});

export const store = createStore(rootReducer);

export type AppDispatch = typeof store.dispatch;

// Add this to verify store is created
console.log("✅ Redux store created:", store);
