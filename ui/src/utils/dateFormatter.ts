export const dateFormatter = {

	format(date: string | undefined): string {
		if (!date) return ''
		const dateObj = new Date(date)
		return new Intl.DateTimeFormat('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric',
			hour: '2-digit',
			minute: '2-digit',
		}).format(dateObj)

	},



}