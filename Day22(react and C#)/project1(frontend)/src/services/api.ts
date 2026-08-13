import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7175/api",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 10000,
});
// API FUNCTIONS

export const submitFormData = async (formData: any) => {
  const response = await api.post("/submissions", formData);
  return response.data;
};

// ✅ This gets all submissions from database
export const getSubmissions = async () => {
  const response = await api.get("/submissions");
  return response.data;
};

export const deleteSubmission = async (id: string) => {
  const response = await api.delete(`/submissions/${id}`);
  return response.data;
};

export const deleteAllSubmissions = async () => {};

export default api;
