# React — Complete Study Notes

> A polished, from-scratch-to-production reference for React fundamentals.
> Format per topic: **Concept → Why It Matters → Notes → Example → Common Mistakes → Mini Exercise → Production Tip**

---

## Table of Contents
1. [Project Setup](#1-project-setup)
2. [Components](#2-components)
3. [Export & Import](#3-export--import)
4. [JSX Rules](#4-jsx-rules)
5. [Props](#5-props)
6. [Conditional Rendering](#6-conditional-rendering)
7. [Rendering Lists & Keys](#7-rendering-lists--keys)
8. [Styling Components](#8-styling-components)
9. [Event Handling](#9-event-handling)
10. [State (props vs state)](#10-state-props-vs-state)
11. [useState Hook](#11-usestate-hook)
12. [The Render Cycle (Trigger → Render → Commit)](#12-the-render-cycle)
13. [Batching](#13-batching)
14. [useState with Objects & Arrays](#14-usestate-with-objects--arrays)
15. [Lifting State Up](#15-lifting-state-up)
16. [useReducer Hook](#16-usereducer-hook)
17. [useState vs useReducer](#17-usestate-vs-usereducer)
18. [Prop Drilling & Context](#18-prop-drilling--context)
19. [Ref & useRef Hook](#19-ref--useref-hook)
20. [Quick Reference Cheat Sheet](#20-quick-reference-cheat-sheet)

---

## 1. Project Setup

### Concept
Vite is the modern build tool used to scaffold React projects — it replaced Create React App (CRA), which is now deprecated.

### Why It Matters
Vite gives near-instant dev server startup and hot reload because it uses native ES modules during development instead of bundling everything upfront like older tools did.

### Notes
```bash
# 1. Create a new project
npm create vite@latest

# 2. You'll be prompted for:
#    - Project name
#    - Framework: React
#    - Variant: JavaScript or TypeScript (TypeScript recommended for real projects)

# 3. Move into the folder & install dependencies
cd your-project-name
npm install

# 4. Run the dev server
npm run dev

# 5. Keep React up to date
npm install react@latest react-dom@latest
npm install -D @types/react@latest @types/react-dom@latest   # TypeScript only
```

### Common Mistakes
- Forgetting to `cd` into the project folder before running `npm install`.
- Mixing up `npm run dev` (development) with `npm run build` (production bundle).
- Not adding a `.gitignore` (Vite includes one by default — don't delete it; it excludes `node_modules`).

### Production Tip
In real companies, you rarely start a repo from scratch each time — a shared **project template** (with ESLint, Prettier, folder structure, and env config pre-set) is used so every new project starts consistent. Consider building your own starter template once you're comfortable.

---

## 2. Components

### Concept
A **component** is a JavaScript function that returns UI (written in JSX). React apps are just trees of components calling other components.

### Why It Matters
Components are the core unit of reuse in React — instead of repeating markup, you build a piece once and reuse it anywhere, with different data each time (via props).

### Notes
- Component names **must** start with a capital letter (PascalCase) — this is how React tells apart a custom component (`<Welcome />`) from a plain HTML tag (`<div />`).
- A component returns JSX describing what should appear on screen.
- Components can be nested inside other components to build complex UIs from small pieces.

### Example
```jsx
// Welcome.jsx
function Welcome() {
  return <h1>Welcome, Sushit!</h1>;
}

// App.jsx
function App() {
  return (
    <div>
      <Welcome />
      <Welcome />
    </div>
  );
}
```

### Common Mistakes
- Naming a component `welcome` (lowercase) — React will try to render it as an HTML tag `<welcome>` and fail silently or throw an error.
- Writing logic-heavy code directly in JSX instead of computing values above the `return` statement.

### Mini Exercise
Create a `Profile` component that returns a `<h2>` with your name, and render it twice inside `App`.

### Solution
```jsx
function Profile() {
  return <h2>Sushit's Profile</h2>;
}

function App() {
  return (
    <>
      <Profile />
      <Profile />
    </>
  );
}
```

### Production Tip
Large companies keep **one component per file**, named exactly like the component (`Profile.jsx` → `Profile`). This makes files predictable to find and import.

---

## 3. Export & Import

### Concept
JavaScript modules let you split code across files. React components are exported from one file and imported into another.

### Why It Matters
Without this, every component would need to live in a single giant file — unmaintainable at any real scale.

### Notes
Two export styles:
| Type | Syntax | Import |
|---|---|---|
| **Named export** | `export const Welcome = () => {}` | `import { Welcome } from "./Welcome"` |
| **Default export** | `export default function Button() {}` | `import Button from "./Button"` |

- A file can have **many** named exports but only **one** default export.
- Default exports can be renamed freely on import; named exports should match (unless aliased with `as`).

### Example
```jsx
// Welcome.jsx
export const Welcome = () => {
  return <h1>Welcome, Sushit!</h1>;
};

// Button.jsx
export default function AmazingButton() {
  return <button>Click me</button>;
}

// App.jsx
import { Welcome } from "./Welcome";
import AmazingButton from "./Button";

function App() {
  return (
    <>
      <Welcome />
      <AmazingButton />
    </>
  );
}
```

### JSX & Fragments
- JSX compiles down to `React.createElement()` calls — JSX is just syntactic sugar, not real HTML.
- `<React.Fragment></React.Fragment>` or the shorthand `<></>` groups elements **without** adding an extra node to the DOM.

### Common Mistakes
- Forgetting curly braces `{}` for named imports (`import Welcome from "./Welcome"` when it was a named export — this imports `undefined`).
- Circular imports (File A imports File B, which imports File A) — causes bugs that are hard to trace.

### Production Tip
Most teams prefer **named exports** across the board (even for single components) because they enforce consistent naming during import and make refactors/renames easier to trace with IDE tools.

---

## 4. JSX Rules

### Concept
JSX is a syntax extension that lets you write HTML-like markup inside JavaScript.

### Why It Matters
Breaking these rules is the #1 source of beginner errors — knowing them cold saves hours of debugging.

### Notes
1. **Single root element** — a component must return one parent element (or a Fragment `<>...</>`).
2. **Every tag must close** — `<img />`, `<br />`, `<input />` (self-closing), and all others need explicit closing tags.
3. **camelCase attributes** — `class` → `className`, `onclick` → `onClick`, `for` → `htmlFor`.
4. **Embed JS with `{}`** — any valid JS expression can go inside curly braces.

### Example
```jsx
function Card() {
  const name = "Sushit";
  return (
    <div className="card">
      <img src="/avatar.png" alt="avatar" />
      <h2>{name}</h2>
      <p>{2 + 2} years of experience</p>
    </div>
  );
}
```

### Common Mistakes
```jsx
// ❌ Wrong: two root elements
return (
  <h1>Title</h1>
  <p>Text</p>
);

// ✅ Correct: wrapped in a Fragment
return (
  <>
    <h1>Title</h1>
    <p>Text</p>
  </>
);
```
```jsx
// ❌ Wrong: statements (like if) can't go inside {}
<p>{ if (loggedIn) { "Hi" } }</p>

// ✅ Correct: use an expression instead (ternary, &&, etc.)
<p>{loggedIn ? "Hi" : "Please log in"}</p>
```

### Production Tip
ESLint's `react/jsx-key` and `react/self-closing-comp` rules are typically enforced in company codebases via CI — code that violates JSX conventions fails the build before it can be merged.

---

## 5. Props

### Concept
Props ("properties") are how a parent component passes data down to a child component.

### Why It Matters
Props make components **reusable and dynamic** — the same `<Button>` component can render "Save", "Cancel", or "Delete" depending on what's passed in.

### Notes
- Props are **read-only** — a child must never modify its own props (this is a core React rule, similar to function arguments being immutable in intent).
- You can pass any JS value as a prop: strings, numbers, arrays, objects, functions, even JSX.
- Access props via the `props` object, or (cleaner) destructure them directly in the function signature.

### Example
```jsx
// Button.jsx
function Button({ label, onClick, variant = "primary" }) {
  return (
    <button className={variant} onClick={onClick}>
      {label}
    </button>
  );
}

// App.jsx
function App() {
  return (
    <Button
      label="Save"
      variant="success"
      onClick={() => console.log("Saved!")}
    />
  );
}
```

### Common Mistakes
- Trying to reassign a prop inside the child: `props.label = "New"` — this breaks React's data flow and won't reliably update the UI.
- Forgetting default values, causing `undefined` to render for optional props.

### Mini Exercise
Build a `Badge` component that accepts a `text` and `color` prop, and renders a colored `<span>`.

### Solution
```jsx
function Badge({ text, color = "gray" }) {
  return <span style={{ backgroundColor: color }}>{text}</span>;
}
```

### Production Tip
In TypeScript projects (the industry standard now), props are typed explicitly:
```tsx
type ButtonProps = {
  label: string;
  onClick: () => void;
  variant?: "primary" | "success" | "danger";
};

function Button({ label, onClick, variant = "primary" }: ButtonProps) {
  return <button className={variant} onClick={onClick}>{label}</button>;
}
```
This catches prop-misuse bugs at compile time instead of at runtime in production.

---

## 6. Conditional Rendering

### Concept
Showing different UI depending on some condition (a boolean, a value, loading state, etc.).

### Why It Matters
Almost every real UI (loading spinners, empty states, error messages, login gates) depends on conditional rendering.

### Notes — 4 Techniques

| Technique | Best For |
|---|---|
| `if` statement (before `return`) | Completely different renders, or returning `null` |
| Ternary `condition ? a : b` | Either/or situations |
| AND operator `condition && <JSX />` | Show/hide a single element |
| Variable holding JSX | Complex logic that would clutter the JSX |

### Example
```jsx
function Status({ isLoading, error, data }) {
  // 1. Early return (if statement)
  if (isLoading) return <p>Loading...</p>;

  // 2. Variable for complex logic
  let content;
  if (error) {
    content = <p className="error">{error}</p>;
  } else {
    content = <p>{data}</p>;
  }

  return (
    <div>
      {/* 3. Ternary */}
      {data ? <h2>Result found</h2> : <h2>No result</h2>}

      {/* 4. AND operator (show/hide) */}
      {error && <span>⚠️ Something went wrong</span>}

      {content}
    </div>
  );
}
```

### Common Mistakes
```jsx
// ❌ Danger: if count is 0, this renders the literal number "0" instead of nothing!
{count && <p>You have {count} items</p>}

// ✅ Fix: make sure the condition is a real boolean
{count > 0 && <p>You have {count} items</p>}
```

### Production Tip
For 3+ possible states (loading / error / empty / success), many teams avoid nested ternaries entirely and use a `switch` statement or a lookup object — nested ternaries are hard to read in code review.

---

## 7. Rendering Lists & Keys

### Concept
Use `.map()` to turn an array of data into an array of JSX elements.

### Why It Matters
Almost all real apps render dynamic lists (products, comments, table rows) — and React needs `key` to track which item is which across re-renders.

### Notes
- **Keys must be unique** among siblings in that list.
- The `key` prop goes on the **outermost** repeated element inside `.map()`.
- Keys let React efficiently figure out which items were added, removed, or reordered — without keys (or with bad keys), React may re-render or lose state incorrectly.

### Example
```jsx
function TodoList({ todos }) {
  return (
    <ul>
      {todos.map((todo) => (
        <li key={todo.id}>{todo.text}</li>
      ))}
    </ul>
  );
}
```

### Index-as-Key Anti-Pattern
Using the array index (`todos.map((todo, index) => <li key={index}>)`) is a common shortcut, but it **breaks** when the list can be reordered, filtered, or items are inserted/removed — React can mismatch state across items.

**Safe to use index as key only when:**
- Items have no unique ID.
- The list is completely static (never added to/removed from).
- The list is never reordered or filtered.
- Example: a hardcoded navigation menu.

### Common Mistakes
```jsx
// ❌ Using array index for a dynamic, reorderable list
{todos.map((todo, i) => <li key={i}>{todo.text}</li>)}

// ✅ Using a stable, unique ID
{todos.map((todo) => <li key={todo.id}>{todo.text}</li>)}
```

### Production Tip
Database-backed data almost always comes with a unique `id` (from SQL primary keys, MongoDB `_id`, etc.) — use that as the key. Never generate a `Math.random()` key inside `.map()`; it changes every render and defeats the purpose entirely.

---

## 8. Styling Components

### Concept
There are 3+ ways to style React components, each with different tradeoffs.

### Notes & Examples

**1. Inline styles** — a JS object, camelCase properties, good for dynamic one-off values.
```jsx
function Alert({ children }) {
  return (
    <div style={{ backgroundColor: "#10b981", color: "black", padding: 12 }}>
      {children}
    </div>
  );
}
```

**2. External CSS files** — plain global CSS, imported into the component file.
```css
/* Alert.css */
.alert {
  padding: 16px;
  border-radius: 8px;
}
```
```jsx
import "./Alert.css";
function Alert({ children }) {
  return <div className="alert">{children}</div>;
}
```

**3. CSS Modules** — scoped CSS, class names are auto-generated to avoid collisions.
```css
/* Alert.module.css */
.alert {
  padding: 16px;
}
```
```jsx
import styles from "./Alert.module.css";
function Alert({ children }) {
  return <div className={styles.alert}>{children}</div>;
}
```

**4. Utility-first CSS (Tailwind)** — the modern industry default for speed.
```jsx
function Alert({ children }) {
  return <div className="p-4 rounded-lg bg-emerald-500 text-black">{children}</div>;
}
```

### Common Mistakes
- Global CSS files causing class-name collisions across components (`.card` defined in two different files, one overrides the other).
- Overusing inline styles for things that never change — this bloats JSX and can't be cached/optimized like a real stylesheet class.

### Production Tip
Large companies typically pick **one** styling strategy per codebase (commonly Tailwind or CSS Modules today) and enforce it via linting — mixing 3 different styling approaches in one app is a common source of tech debt.

---

## 9. Event Handling

### Concept
React wraps native browser events into "SyntheticEvents" so behavior is consistent across browsers, using camelCase handler props like `onClick`, `onChange`, `onSubmit`.

### Notes
| Event | Fires On |
|---|---|
| `onClick` | Button/element click |
| `onChange` | Input value changes |
| `onSubmit` | Form submission |
| `onMouseEnter` / `onMouseLeave` | Hover in/out |
| `onKeyDown` / `onKeyUp` | Keyboard input |
| `onFocus` / `onBlur` | Input focus gained/lost |

### Example
```jsx
function LikeButton() {
  const handleClick = () => {
    alert("Thanks for liking!");
  };
  return <button onClick={handleClick}>Like</button>;
}

function SearchInput() {
  const handleChange = (event) => {
    console.log("User typed:", event.target.value);
  };
  return <input onChange={handleChange} placeholder="Search..." />;
}
```

### Passing Data From Child to Parent
Since a child can't "return" data outward, it calls a function (passed down as a prop) to tell the parent "this happened" — the parent decides what to do.

```jsx
// Child
function DeleteButton({ onDelete }) {
  return <button onClick={onDelete}>Delete</button>;
}

// Parent
function TodoItem({ todo, onRemove }) {
  return (
    <div>
      {todo.text}
      <DeleteButton onDelete={() => onRemove(todo.id)} />
    </div>
  );
}
```

### Common Mistakes
```jsx
// ❌ Calling the function immediately during render (runs on every render!)
<button onClick={handleClick()}>Like</button>

// ✅ Passing a reference to the function
<button onClick={handleClick}>Like</button>

// ✅ Or wrapping in an arrow function if you need to pass arguments
<button onClick={() => handleDelete(todo.id)}>Delete</button>
```

### Production Tip
For forms, production apps rarely hand-roll all validation — libraries like **React Hook Form** + **Zod/Yup** handle validation, error messages, and submission state cleanly at scale.

---

## 10. State (props vs state)

### Concept
| | Props | State |
|---|---|---|
| Analogy | Function arguments | A component's private memory |
| Who sets it | Parent component | The component itself |
| Can it change? | No (read-only) | Yes, via a setter function |
| Where does it come from | Passed down | Created internally |

### Why It Matters
Understanding this distinction is the single most important mental model in React — nearly every bug for beginners traces back to confusing "data that flows in" (props) with "data the component owns and updates" (state).

---

## 11. useState Hook

### Concept
`useState` gives a component memory that persists between renders and triggers a re-render when updated.

### Notes
```jsx
const [count, setCount] = useState(0);
```
- Returns an array: `[currentValue, setterFunction]`.
- Calling `setCount(newValue)`:
  1. React schedules a state update.
  2. React re-renders the component.
  3. `useState` returns the **new** value on that render.
  4. The UI reflects the updated value.

### Lazy Initialization
If the initial value requires an expensive calculation, pass a **function** instead of a value — it only runs once, on mount.
```jsx
const [data, setData] = useState(() => expensiveComputation());
```

### The Two Golden Rules of Hooks
1. **Only call hooks at the top level** — never inside:
   - loops
   - conditions (`if` statements)
   - nested functions
   - `try/catch` blocks
2. **Only call hooks from React functions** — either React components, or custom hooks (functions starting with `use`).

### Example
```jsx
function Counter() {
  const [count, setCount] = useState(0);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => setCount(count + 1)}>+1</button>
      <button onClick={() => setIsLoggedIn(!isLoggedIn)}>
        {isLoggedIn ? "Log out" : "Log in"}
      </button>
    </div>
  );
}
```

### Value vs Updater Function
```jsx
// ❌ All three use the same stale snapshot of `count` from this render — result: +1, not +3
setCount(count + 1);
setCount(count + 1);
setCount(count + 1);

// ✅ Each builds on the true latest value — result: +3
setCount((prev) => prev + 1);
setCount((prev) => prev + 1);
setCount((prev) => prev + 1);
```
**Rule of thumb:** if the new state depends on the *previous* state, always use the updater-function form.

### Common Mistakes
- Calling a hook conditionally: `if (x) { const [a, setA] = useState(0); }` — breaks React's internal hook order tracking and crashes the app.
- Mutating state directly instead of calling the setter (see next section).

### Production Tip
ESLint's `eslint-plugin-react-hooks` catches rule-of-hooks violations automatically and is a mandatory part of most companies' CI pipelines.

---

## 12. The Render Cycle

### Concept
Every UI update in React goes through three phases:

```
1. TRIGGER   → something calls a setter (setState) or it's the initial render
2. RENDER    → React calls your component function to figure out what the UI should look like
3. COMMIT    → React applies the minimal set of changes to the actual DOM
```

### Why It Matters
Understanding this prevents a very common misconception: calling a state setter does **not** update the DOM immediately — it schedules a re-render, and the DOM only changes after the commit phase.

```jsx
function Example() {
  const [count, setCount] = useState(0);

  function handleClick() {
    setCount(count + 1);
    console.log(count); // ❌ Still logs the OLD value — this render's snapshot hasn't changed
  }

  return <button onClick={handleClick}>{count}</button>;
}
```

---

## 13. Batching

### Concept
React groups (**batches**) multiple state updates that happen within the same event handler into a single re-render, instead of re-rendering after every single `setState` call.

### Why It Matters
Without batching, every setter call would trigger a separate render — wasteful and could cause UI flicker/inconsistent intermediate states.

### Notes
```
React's process:
1. Wait until your event handler function finishes running.
2. Gather all the state updates that were requested.
3. Apply them together in ONE re-render.
```

### Example
```jsx
function handleClick() {
  setCount((c) => c + 1);
  setFlag((f) => !f);
  // Even though 2 setters were called, React only re-renders ONCE.
}
```

### Production Tip
Since React 18, batching also applies to updates inside promises, `setTimeout`, and native event handlers (not just React's synthetic events) — this was a significant behavior change from React 17 worth knowing if you read older tutorials.

---

## 14. useState with Objects & Arrays

### Concept
React detects updates using **reference equality**, not deep comparison — so you must always create a **new** object/array, never mutate the existing one.

### Why It Matters
Mutating state directly (e.g. `state.name = "new"`) doesn't change the object's reference, so React doesn't know anything changed — the UI silently fails to re-render, which is a very confusing bug to track down.

### Objects
```jsx
const [user, setUser] = useState({ name: "Sushit", age: 22 });

// ❌ Wrong: direct mutation, no re-render triggered
user.age = 23;

// ✅ Correct: spread the old state, override the changed field
setUser((prev) => ({ ...prev, age: 23 }));
```
> Calling `setState` with an object **replaces the entire object** — it does not automatically merge, unlike old-school `this.setState` in class components.

### Arrays
```jsx
const [items, setItems] = useState([1, 2, 3]);

// Add an item
setItems((prev) => [...prev, 4]);              // spread
setItems((prev) => prev.concat(4));            // concat

// Remove an item
setItems((prev) => prev.filter((item) => item !== 2));

// Update an item
setItems((prev) => prev.map((item) => (item === 2 ? 20 : item)));
```

### Common Mistakes
```jsx
// ❌ push mutates the original array in place
items.push(4);
setItems(items);

// ✅ Create a new array instead
setItems([...items, 4]);
```

### Production Tip
For deeply nested state objects, manual spreading gets messy fast (`{...state, a: {...state.a, b: {...state.a.b, c: 1}}}`). Production codebases often use a library like **Immer** (`useImmer`), which lets you write "mutating-looking" code that's actually immutable under the hood.

---

## 15. Lifting State Up

### Concept
When two or more sibling components need to share the same data, move that state to their **closest common parent**, which then passes it down via props.

### Why It Matters
This is React's core pattern for sharing state without external libraries — the parent becomes the single source of truth.

### Example
```jsx
function App() {
  const [selectedTab, setSelectedTab] = useState("home");

  return (
    <>
      <TabBar selected={selectedTab} onSelect={setSelectedTab} />
      <TabContent selected={selectedTab} />
    </>
  );
}

function TabBar({ selected, onSelect }) {
  return (
    <div>
      <button onClick={() => onSelect("home")}>Home</button>
      <button onClick={() => onSelect("profile")}>Profile</button>
    </div>
  );
}

function TabContent({ selected }) {
  return <p>Showing: {selected}</p>;
}
```

### Production Tip
Lifting state up works great for a few levels — beyond that, it turns into prop drilling (next topic). Knowing *when* to stop lifting and reach for Context or a state library (Redux, Zustand) is a key architectural skill.

---

## 16. useReducer Hook

### Concept
`useReducer` is an alternative to `useState` for managing more complex state logic, based on the **reducer pattern**: `(state, action) => newState`.

### Why It Matters
When state updates involve multiple sub-values, branching logic, or updates that depend on each other, `useReducer` keeps the update logic centralized and predictable instead of scattered across many `setX` calls.

### Notes — Mechanics
```
1. An ACTION describes what happened (e.g. { type: "INCREMENT" }).
2. DISPATCH sends that action to the reducer.
3. The REDUCER decides how to compute the new state from (state, action).
4. The component RE-RENDERS with the new state.
```

- `useReducer(reducer, initialState)` — takes a reducer function and an initial state.
- An action is just an instruction object; it can carry extra data via a `payload` field.
- Reducer signature: `(state, action) => newState`.

### Example
```jsx
const initialState = { count: 0 };

function reducer(state, action) {
  switch (action.type) {
    case "INCREMENT":
      return { count: state.count + 1 };
    case "DECREMENT":
      return { count: state.count - 1 };
    case "SET":
      return { count: action.payload };
    default:
      throw new Error(`Unknown action: ${action.type}`);
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, initialState);

  return (
    <div>
      <p>{state.count}</p>
      <button onClick={() => dispatch({ type: "INCREMENT" })}>+</button>
      <button onClick={() => dispatch({ type: "DECREMENT" })}>-</button>
      <button onClick={() => dispatch({ type: "SET", payload: 100 })}>Set to 100</button>
    </div>
  );
}
```

### Lazy Initialization (3rd argument)
```jsx
function init(initialCount) {
  // expensive setup work, runs ONCE
  return { count: initialCount };
}

const [state, dispatch] = useReducer(reducer, 0, init);
// React calls init(0) once on mount — not on every re-render.
```

### useState is Built on useReducer
- `useState` is actually implemented internally using the same reducer principle — `useReducer` is the more fundamental, lower-level hook.
- Every reducer, at its core, takes `(state, action)` and returns new state.

### Common Mistakes
- Mutating `state` directly inside the reducer instead of returning a new object.
- Forgetting a `default` case in the `switch`, silently swallowing unknown actions.

### Production Tip
This exact pattern (action types, action creators, switch-case reducers) is the foundation of **Redux** — learning `useReducer` well makes learning Redux (or Redux Toolkit) far easier, since Redux is essentially `useReducer` at the whole-app level with extra tooling.

---

## 17. useState vs useReducer

| Use `useState` when... | Use `useReducer` when... |
|---|---|
| You have simple, independent values | You're managing complex objects/arrays |
| State updates are straightforward | You have 4+ related state values |
| You're managing 1–3 pieces of related state | Updates involve branching/complex logic |
| You want the simplest solution that works | Multiple values must update together atomically |
| — | Debugging scattered `setX` calls is becoming difficult |

---

## 18. Prop Drilling & Context

### Concept — Prop Drilling
Passing props through several layers of components that don't need the data themselves, just to reach a deeply nested child that does.

### Why It's a Problem
1. **Maintenance** — every intermediate component has to know about and forward props it doesn't use.
2. **Performance** — can cause unnecessary re-renders of components that don't actually use the data.

### Concept — Context
Context lets you broadcast a value from a high-level component, and any nested component can subscribe to it directly — skipping the intermediate layers entirely.

### Example
```jsx
// 1. Create the context
const ThemeContext = createContext("light");

// 2. Provide it at a high level
function App() {
  return (
    <ThemeContext.Provider value="dark">
      <Toolbar />
    </ThemeContext.Provider>
  );
}

// 3. No need to pass "theme" through Toolbar!
function Toolbar() {
  return <ThemedButton />;
}

// 4. Consume it directly, however deep it is
function ThemedButton() {
  const theme = useContext(ThemeContext);
  return <button className={theme}>Click</button>;
}
```

### Context + State
Context isn't limited to static values — combine it with `useState` (or `useReducer`) so nested components can both **read and update** shared state.

```jsx
const ThemeContext = createContext();

function App() {
  const [theme, setTheme] = useState("light");
  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      <Toolbar />
    </ThemeContext.Provider>
  );
}

function ThemedButton() {
  const { theme, setTheme } = useContext(ThemeContext);
  return (
    <button onClick={() => setTheme(theme === "light" ? "dark" : "light")}>
      Current: {theme}
    </button>
  );
}
```

### `use()` API (React 19)
- `use()` is a newer API that can read context (and promises) but is **not a hook** — meaning it doesn't have to follow the Rules of Hooks (e.g., it can be called conditionally).

### Common Mistakes
- Wrapping the **entire app** in one giant Context for everything — causes every consumer to re-render on any change, even unrelated ones. Split contexts by concern (e.g., `AuthContext`, `ThemeContext`) instead.
- Forgetting the `.Provider` wrapper and getting the default value everywhere instead of the real one.

### Production Tip
Context is great for **low-frequency global data** (theme, logged-in user, locale). For **high-frequency, complex app state** (large forms, real-time data, big normalized datasets), production apps typically reach for Redux Toolkit, Zustand, or React Query instead — Context alone can cause performance issues at scale because every consumer re-renders on any value change.

---

## 19. Ref & useRef Hook

### Concept
Refs store a value that persists across renders **without** triggering a re-render when changed — unlike state.

### Why It Matters
| | State | Ref |
|---|---|---|
| Triggers re-render on change | ✅ Yes | ❌ No |
| Used for | Data the UI displays | Data you need to remember, but not show |
| Common use | Form input value, toggle, counters | DOM element access, timers, previous values |

### Notes
- Create with `useRef(initialValue)` — returns an object `{ current: initialValue }`.
- Update it via `ref.current = newValue` — this does **not** cause a re-render.
- A very common use: getting **direct access to a DOM element**.

### Example
```jsx
function TextInputWithFocusButton() {
  const inputRef = useRef(null);

  function handleClick() {
    inputRef.current.focus(); // directly manipulating the DOM
  }

  return (
    <>
      <input ref={inputRef} type="text" />
      <button onClick={handleClick}>Focus the input</button>
    </>
  );
}
```

### Common Mistakes
- Using a ref to store data that should actually update the UI (e.g., a counter shown on screen) — since refs don't trigger re-renders, the UI won't update even though the value changed internally.
- Reading/writing `ref.current` during render (should only be done in event handlers or `useEffect`).

### Production Tip
Refs are frequently used for: focus management, scroll position tracking, integrating with third-party non-React libraries (like chart libraries), and storing timer/interval IDs for cleanup.

---

## 20. Quick Reference Cheat Sheet

```
┌─────────────────────────────────────────────────────────┐
│  WHEN TO USE WHAT                                        │
├─────────────────────────────────────────────────────────┤
│  Need to display data that changes → useState             │
│  Complex/branching state logic     → useReducer            │
│  Share data across many components → Context               │
│  Store a value without re-render   → useRef                │
│  Reach into the DOM directly       → useRef                │
│  Pass data parent → child          → props                 │
│  Notify parent from child          → callback prop         │
└─────────────────────────────────────────────────────────┘
```

**Golden Rules Recap:**
- Props flow down, events flow up.
- Never mutate state directly — always create new objects/arrays.
- Hooks only at the top level, only in React functions.
- If state depends on previous state → use the updater-function form.
- Keys in lists must be stable and unique — avoid array index for dynamic lists.

---

*Notes compiled and expanded for Sushit — Full-Stack Development Study Track (React → C#/.NET → SQL Server)*
