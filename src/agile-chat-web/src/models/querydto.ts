export interface QueryDto {
    search?: string;
    orderBy?: string;
    orderType?: OrderType;
    page: number;
    pageSize: number;
  }

export enum OrderType{
    ASC = "ASC",
    DESC = "DESC"
}