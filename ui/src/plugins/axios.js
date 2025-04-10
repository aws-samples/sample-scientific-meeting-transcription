/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

// src/plugins/axios.js
// This file configures axios for API calls with authentication and error handling

// Import required dependencies
import axios from 'axios'
import router from '@router' // Import Vue router for navigation
import { fetchAuthSession } from 'aws-amplify/auth'; // Import Amplify auth for token management

// Create a custom axios instance with default configuration
const axiosInstance = axios.create({
  withCredentials: false, // Don't send cookies with cross-origin requests
  baseURL: import.meta.env.VITE_API_URL, // Base API URL from environment variables
  timeout: 60000, // 60 second timeout for requests
  headers: {
    'Content-Type': 'application/json', // Default content type
   // 'x-apigw-api-id': import.meta.env.VITE_API_ID // API Gateway ID for request tracking
  },
})


// Request interceptor - runs before each request is sent
axiosInstance.interceptors.request.use(
  async config => {
    // Remove null parameters from request
    if (config.params) {
      config.params = Object.fromEntries(
        Object.entries(config.params).filter(([, value]) => value !== null)
      );
    }
    
    // Get current auth session and add ID token to Authorization header
    const session = await fetchAuthSession();
    config.headers.Authorization = `Bearer ${session.tokens?.idToken?.toString()}`

     // Log request details
    console.log('🚀 Request:', {
      method: config.method?.toUpperCase(),
      url: config.url,
      baseURL: config.baseURL,
      params: config.params,
      headers: {
        ...config.headers,
        Authorization: '[REDACTED]' // Don't log sensitive auth tokens
      },
      timestamp: new Date().toISOString()
    });

    return config

  },
  error => {
    // Handle request errors
    return Promise.reject(error)
  }
)

// Response interceptor - runs after each response is received
axiosInstance.interceptors.response.use(
  (response) => {
    // Pass through successful responses
    return response
  },
  (error) => {
    // Handle error responses
    if (error.response) {
      // The server responded with a status code outside of 2xx
      console.log('Error response:', error.response)
      console.log('Error status:', error.response.status)
      console.log('Error data:', error.response.data)
    } else if (error.message === 'Network Error' || error.response?.status === 401) {
      // Handle authentication errors (401) or network errors
      console.log('Handling 401 error')
      localStorage.removeItem('token') // Clear stored token
      router.push('/login') // Redirect to login page
    }
    return Promise.reject(error)
  }
)

// Debug logging for response handlers
axiosInstance.interceptors.response.handlers.forEach(handler => {
  // This is empty but could be used for debugging interceptors
})

// Export the configured axios instance for use throughout the application
export default axiosInstance
