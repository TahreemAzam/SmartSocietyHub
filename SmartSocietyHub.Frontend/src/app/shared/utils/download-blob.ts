// Blobs from authenticated endpoints (invoices, payment proofs) can't be
// linked to directly with a plain <a href> — the browser wouldn't send
// the Authorization header. Fetch as a Blob via HttpClient instead, then
// use this to save or open it.
export function downloadBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
