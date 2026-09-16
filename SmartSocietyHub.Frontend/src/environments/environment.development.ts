export const environment = {
  production: false,
  // API's HTTP profile (see SmartSocietyHub.API/Properties/launchSettings.json).
  // Using HTTP instead of the HTTPS profile in local dev avoids the browser
  // blocking requests behind an untrusted local dev certificate.
  apiBaseUrl: 'http://localhost:5181/api',
};
