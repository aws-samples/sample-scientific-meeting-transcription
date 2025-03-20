/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

// src/plugins/axios.js
import axios from 'axios'
import router from '@router' // Make sure to import your router
import { fetchAuthSession } from 'aws-amplify/auth';

const axiosInstance = axios.create({
  withCredentials: false,
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json',
    'x-apigw-api-id': import.meta.env.VITE_API_ID
  },
})


// Request interceptor
axiosInstance.interceptors.request.use(
  async config => {
    if (config.params) {
      config.params = Object.fromEntries(
        Object.entries(config.params).filter(([, value]) => value !== null)
      );
    }
    const session = await fetchAuthSession();
    config.headers.Authorization = `Bearer ${session.tokens?.idToken?.toString()}`
    return config
  },
  error => {
    return Promise.reject(error)
  }
)

axiosInstance.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    // Add more detailed logging
    if (error.response) {
      // The server responded with a status code outside of 2xx
      console.log('Error response:', error.response)
      console.log('Error status:', error.response.status)
      console.log('Error data:', error.response.data)
    } else if (error.message === 'Network Error' || error.response?.status === 401) {
      console.log('Handling 401 error')
      localStorage.removeItem('token')
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

axiosInstance.interceptors.response.handlers.forEach(handler => {
})

export default axiosInstance
