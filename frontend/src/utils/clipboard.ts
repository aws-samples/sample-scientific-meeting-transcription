export async function copyToClipboard(text: string): Promise<void> {
  try {
    await navigator.clipboard.writeText(text);
    return Promise.resolve();
  } catch (err) {
    console.error('Failed to copy text: ', err);
    return Promise.reject(err);
  }
}