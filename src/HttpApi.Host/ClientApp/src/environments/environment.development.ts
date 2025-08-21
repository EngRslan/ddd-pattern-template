export const environment = {
  production: false,
  auth: {
    authority: 'https://localhost:7222',
    clientId: 'certmanager-angular-client',
    redirectUri: window.location.origin + '/auth-callback',
    postLogoutRedirectUri: window.location.origin + '/',
    silentRedirectUri: window.location.origin + '/silent-refresh',
    responseType: 'code',
    scope: 'openid profile email certmanager-api',
    automaticSilentRenew: true,
    loadUserInfo: true
  },
  apiUrl: 'https://localhost:7228/api'
};
