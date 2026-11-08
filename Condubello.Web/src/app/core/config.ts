/**
 * Dirección del gateway: el frontend solo habla con él, nunca directamente con la API.
 * Detrás de un túnel de VS Code (`<id>-4200.<región>.devtunnels.ms`) usa el puerto 5000 del mismo túnel.
 */
const host = typeof location === 'undefined' ? '' : location.hostname;
export const API_URL = host.endsWith('.devtunnels.ms')
  ? `https://${host.replace('-4200.', '-5000.')}`
  : 'http://localhost:5000';
