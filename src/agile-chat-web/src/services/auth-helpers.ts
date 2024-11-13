// auth-helpers.ts
import { useMsal, useAccount } from '@azure/msal-react';
import { AccountInfo } from '@azure/msal-browser';

export const isLoggedIn = (accounts: AccountInfo[]): boolean => {
  return accounts && accounts.length > 0;
};

export const getName = (account: AccountInfo | null): string => {
  return account ? account.name! : '';
};

export const getUserName = (account: AccountInfo | null): string => {
  return account ? account.username : '';
};

export const useAuth = () => {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0] || null);

  return {
    instance,
    accounts,
    account,
    isLoggedIn: isLoggedIn(accounts),
    name: getName(account),
    username: getUserName(account),
  };
};
