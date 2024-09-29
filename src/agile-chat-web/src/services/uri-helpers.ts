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
  