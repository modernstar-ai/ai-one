import React from 'react'
import { Button } from "@/components/ui/button"
import { Home, History, MessageSquare, Settings, VenetianMask, MessageSquareCode, FileBox } from 'lucide-react'
import { Link } from 'react-router-dom';

interface LeftMenuProps {
    isHistoryOpen: boolean;
    setIsHistoryOpen: React.Dispatch<React.SetStateAction<boolean>>;
}

const LeftMenu: React.FC<LeftMenuProps> = ({ isHistoryOpen, setIsHistoryOpen }) => {


    return (
        <div className="w-16 bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4">
            <Link aria-label="Home" to="/" accessKey="h"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Home Button"><Home className="h-6 w-6" /></Button></Link>
            <Link aria-label="Chat" to="/chat" accessKey="c"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chat Button"><MessageSquare className="h-6 w-6" /></Button></Link>
            <Link aria-label="Chat over data" to="/ragchat" accessKey="r"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chat over Data Button"><MessageSquareCode className="h-6 w-6" /></Button></Link>
            <Button aria-label="Chat History" variant="ghost" size="icon" onClick={() => setIsHistoryOpen(!isHistoryOpen)} accessKey="s">
                <History className="h-6 w-6" />
            </Button>
            <Link aria-label="Personas" to="/personas" accessKey="p"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Personas Button"><VenetianMask className="h-6 w-6" /></Button></Link>
            <Link to="/fileupload"><Button variant="ghost" size="icon"><MessageSquare className="h-6 w-6" /></Button></Link>
            
            <Link aria-label="File Management" to="/files" accessKey="l"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Files Button"><FileBox className="h-6 w-6" /></Button></Link>
            <Link aria-label="Tools" to="/tools" accessKey="t"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Tools Button"><Settings className="h-6 w-6" /></Button></Link>
        </div>
    );
};

export default LeftMenu;