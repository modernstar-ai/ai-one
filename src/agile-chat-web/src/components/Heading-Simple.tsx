import React from 'react'
import { Button } from "@/components/ui/button"
import { Menu, Paperclip} from 'lucide-react'

interface SimpleHeadingProps {
    Title: string;
    Subtitle: string;
    DocumentCount: number;    
}

const SimpleHeading: React.FC<SimpleHeadingProps> = ({Title, Subtitle, DocumentCount}) => {
    return (
        <div className="bg-muted p-4 flex justify-between items-center w-full">
          <div>
            <h1 className="text-2xl font-bold">{Title}</h1>
            <p className="text-sm">{Subtitle}</p>
          </div>
          <div className="flex space-x-2">
            <Button variant="outline" size="icon" aria-label="Chat Options" ><Menu className="h-4 w-4" /></Button>
            <Button variant="outline" size="icon" aria-label="Chat Attachments"><Paperclip className="h-4 w-4 mr-1" />{DocumentCount}</Button>
          </div>
        </div>
    );
};

export default SimpleHeading;