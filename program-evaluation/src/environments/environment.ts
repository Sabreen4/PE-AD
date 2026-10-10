export const environment = {
  production: false,
  // Single Ocelot API Gateway base URL. Every request path already includes
  // the per-service prefix (/identity, /metadata, /notifications, /k1-setup,
  // /k1-processing) in each service file, so only the gateway origin changes between environments.
  gatewayUrl: 'http://localhost:5199'
};
