import React from 'react';
import { Settings2 } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Label } from '@/components/ui/label';

interface ChatConfigProps {
  thread: {
    temperature: string;
    topP: string;
    maxResponseToken: string;
    strictness: string;
    documentLimit: string;
  };
}

const ChatConfigPanel: React.FC<ChatConfigProps> = ({ thread }) => {
  const formatValue = (value: string) => {
    return value?.trim() ? value : 'Not set';
  };

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="outline" size="icon" className="ml-2" aria-label="Open settings">
          <Settings2 className="h-4 w-4" />
        </Button>
      </SheetTrigger>
      <SheetContent>
        <SheetHeader>
          <SheetTitle>Chat Configuration</SheetTitle>
        </SheetHeader>
        <div className="mt-6 space-y-4">
          {Object.entries({
            temperature: thread.temperature,
            topP: thread.topP,
            maxResponseToken: thread.maxResponseToken,
            strictness: thread.strictness,
            documentLimit: thread.documentLimit,
          }).map(([key, value]) => (
            <Card key={key} className="border-none">
              <CardContent className="p-3">
                <Label className="text-sm font-medium text-muted-foreground">
                  {key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1')}
                </Label>
                <div className="mt-1 text-sm">{formatValue(value)}</div>
              </CardContent>
            </Card>
          ))}
        </div>
      </SheetContent>
    </Sheet>
  );
};

export default ChatConfigPanel;
