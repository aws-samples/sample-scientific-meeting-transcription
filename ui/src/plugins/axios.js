/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
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
