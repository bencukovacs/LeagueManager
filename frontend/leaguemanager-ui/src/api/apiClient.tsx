import axios from 'axios';

const apiClient = axios.create({
  // This is the URL where your backend API is running
  baseURL: 'http://localhost:8080/api', 
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;