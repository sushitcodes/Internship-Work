// raw HTTP client config (infrastructure)
import axios from "axios";
const axiosInstance = axios.create({
  baseURL: "https://localhost:7000/api",
  headers: {
    "Content-Type": "application/json",
  },
});

export default axiosInstance;
