import { generatePath } from "react-router-dom";
import { Paths, type PathKey } from "./paths";

// Route TEMPLATES have placeholders ("/users/:id"). Route USAGE needs the
// real value ("/users/a3f9..."). This is the one function that turns one
// into the other — react-router's own generatePath does the substitution,
// this just gives it your Paths object's typing so a typo'd key is a
// compile error instead of a page that 404s at runtime.

export function getModuleUrls<K extends PathKey>(
  key: K,
  params?: Record<string, string | number>,
): string {
  return generatePath(Paths[key], params as any);
  Paths[key];
}
//
//the key converts "a string that might point to a route" into "a typed
//  identifier that TypeScript can verify against the real list of routes,"
// so a typo'd route name becomes a compile-time error you fix in seconds
// instead of a runtime 404 your users find.
