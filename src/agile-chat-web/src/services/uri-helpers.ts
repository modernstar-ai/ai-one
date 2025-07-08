export function getApiUri(endpoint: string, params?: Record<string, string | number>): string {
    const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
    console.log('rootApiUrl:', rootApiUrl);
    
    let url = `${rootApiUrl}/${endpoint}`;
    console.log('url:', url);
    
    if (params && Object.keys(params).length > 0) {
      const queryString = new URLSearchParams(params as Record<string, string>).toString();
      url = `${url}?${queryString}`;
    }
  
    return url;
  }
  
  export function getRagApiUri(endpoint: string, prompt: string=""): string {
    const rootApiUrl = import.meta.env.VITE_AGILECHAT_RAGAPI_URL as string;
    console.log('root Api Url:', rootApiUrl);
    
    if (!prompt) {
      const url = `${rootApiUrl}/${endpoint}`;
      console.log('url:', url);  
      return url;
    }
    else {
      const url = `${rootApiUrl}/${endpoint}/${prompt}`;
      console.log('url:', url);  
      return url;
    }
    
  }
  