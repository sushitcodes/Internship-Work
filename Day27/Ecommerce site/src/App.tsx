import { Routes, Route } from "react-router-dom";
import "./App.css";
import Home from "./pages/Home";
import Auth from "./pages/Auth";
import Checkout from "./pages//Checkout";
import Navbar from "./components/Navbar";

function App() {
  return (
    <div className="app">
      <Routes>
        <Route path="/" element={<Home />}></Route>
        <Route path="/auth" element={<Auth />}></Route>
        <Route path="/checkout" element={<Checkout />}>
          {" "}
        </Route>
      </Routes>
    </div>
  );
}

export default App;
