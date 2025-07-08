import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { msalScopes } from '@/authentication/msal-configs';
import { InteractionStatus } from '@azure/msal-browser';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { Loader2Icon } from 'lucide-react';
import { Navigate } from 'react-router-dom';
import { useSettingsStore } from '@/stores/settings-store';

const LoginPage = () => {
  const { instance, inProgress } = useMsal();
  const loggedIn = useIsAuthenticated();
  const { settings } = useSettingsStore();

  if (inProgress === InteractionStatus.Startup || inProgress === InteractionStatus.Logout) {
    return (
      <div className="flex w-full h-screen justify-center items-center bg-background text-foreground">
        <Loader2Icon className="animate-spin" size={64} />
      </div>
    );
  }

  if (loggedIn) {
    return <Navigate to="/" />;
  }

  return (
    <div className="flex w-full  h-[95vh] lg:h-screen justify-center items-center bg-background text-foreground">
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl text-center lg:text-left">
            <span className="text-primary">
              {settings?.appName && settings.appName != '' ? settings.appName : 'Agile Chat'}
            </span>
          </CardTitle>
          <CardDescription>Login in with your Microsoft 365 account</CardDescription>
        </CardHeader>
        <CardContent>
          {inProgress === InteractionStatus.Login ? (
            <div className="flex">
              Login is currently in progress <Loader2Icon className="ml-auto animate-spin" />
            </div>
          ) : (
            <Button className="w-full" onClick={() => instance.loginPopup({ scopes: msalScopes })}>
              Login
            </Button>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default LoginPage;
