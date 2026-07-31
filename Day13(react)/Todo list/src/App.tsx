import { useState, useEffect } from "react";
import "./App.css";

type ApiTodo = {
  id: number;
  title: string;
  completed: boolean;
};

function App() {
  const [todos, setTodos] = useState<string[]>([]);
  const [inputValue, setInputValue] = useState("");
  const [apiTodos, setApiTodos] = useState<ApiTodo[]>([]);

  const handleAdd = () => {
    setTodos([...todos, inputValue]);
    setInputValue("");
  };

  const handleDelete = (indexToRemove: number) => {
    setTodos(todos.filter((_, index) => index !== indexToRemove));
  };

  useEffect(() => {
    const fetchTodos = async () => {
      const response = await fetch(
        "https://jsonplaceholder.typicode.com/todos?_limit=5",
      );
      const data = await response.json();
      setApiTodos(data);
    };
    fetchTodos();
  }, []);

  return (
    <div className="app">
      <h1 className="title">🎨Todo List</h1>
      <div className="input-row">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="What are the Task I need to do?"
          className="todo-input"
        />
        <button onClick={handleAdd} className="add-btn">
          Add
        </button>
      </div>
      <ul className="todo-list">
        {todos.map((todo, index) => (
          <li key={index} className="todo-item">
            <span>{todo}</span>
            <button onClick={() => handleDelete(index)} className="delete-btn">
              Delete
            </button>
          </li>
        ))}
      </ul>

      <h2 className="subtitle">From API</h2>
      <ul className="todo-list api-list">
        {apiTodos.map((todo) => (
          <li key={todo.id}>{todo.title}</li>
        ))}
      </ul>
    </div>
  );
}

// function Greeting(props: { name: string }) {
//   return <h2>Hello,{props.name}!</h2>;
// }
// function App() {
//   const names = ["Sushit", "Roshan", "Sush", "Shyam", "Ram"];
//   return (
//     <>
//       <div className="container">
//         <h1>Hello Sushit</h1>
//         {names.map((name) => (
//           <Greeting key={name} name={name} />
//         ))}
//       </div>
//     </>
//   );
// }
export default App;
