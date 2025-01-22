export interface PagedResultsDto<T> {
    page: number;
    pageSize: number;
    totalCount: number;
    items: T[];
  }
  