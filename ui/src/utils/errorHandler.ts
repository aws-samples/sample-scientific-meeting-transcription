// src/utils/errorHandler.ts
import { AxiosError } from 'axios'


export const errorHandler = {
	handle(error: AxiosError): any {
		if (error.response) {
			console.log(error)
			// Server responded with error
			const status = error.response.status
			const data = error.response.data as any

			switch (status) {
				case 401:
					return 'Your session has expired. Please log in again.'
				case 403:
					return 'You do not have permission to access this resource.'
				// case 404:
				// 	return data.message
				// case 422:
				// 	return data.message
				case 500:
					return 'An internal server error occurred.'
				default:
					return data || 'An unexpected error occurred.'
			}
		} else if (error.request) {
			// Network error
			'Unable to connect to the server. Please check your internet connection.'
		} else {
			'An error occurred while setting up the request.'
		}
	}
}
