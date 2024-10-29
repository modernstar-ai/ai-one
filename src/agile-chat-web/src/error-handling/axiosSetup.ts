import axios from 'axios';
import { errorHandler } from './errorHandler';

const axiosInstance = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL,
  timeout: 10000,
});

// Request interceptor
axiosInstance.interceptors.request.use(
    (config) => { 
      return config;
    },
    (error) => {
      errorHandler.handleError(error);
      return Promise.reject(error);
    }
  );
  
  // Response interceptor
  axiosInstance.interceptors.response.use(
    (response) => response,
    (error) => {
      const status = error.response?.status || 500;
      const context = {
        componentName: 'API_Call',
        action: error.config?.url,
        additionalData: {
          method: error.config?.method,
          url: error.config?.url,
          params: error.config?.params,
        }
      };
  
      const appError = errorHandler.createHttpError(status, context);
      errorHandler.handleError(appError);
      return Promise.reject(appError);
    }
  );
  
  export default axiosInstance;