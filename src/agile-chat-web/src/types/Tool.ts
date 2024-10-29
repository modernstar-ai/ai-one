// // src/types/Tool.ts
// export interface Tool {
//     id: number;  
//     name: string; 
//     status: string; 
//     type: string; 
//     description?: string;
//   }
  
 
  // Tool.ts
export type ToolStatus = 'Active' | 'Inactive' | 'Deprecated';

export type MethodType = 'GET' | 'POST' ;

export type Tool = {
  id: string;
  name: string;
  type: 'Database' | 'LogicApp' | 'ExternalAPI';
  status: ToolStatus;
  description?: string;
  createddate :  string;
  lastupdateddate : string;
  jsonTemplate: string;
  databaseDSN: string;
  databaseQuery: string;
  method: MethodType;
  api: string;

};

 