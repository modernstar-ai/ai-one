import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog';

interface Props {
  label: string | React.ReactNode;
  disabled: boolean;
  children: React.ReactNode;
  title: string;
  description: string;
  showButtons?: boolean;
}

export function BaseDialog(props: Props) {
  const { label, disabled, children, title, description, showButtons } = props;

  return (
    <Dialog>
      <form>
        <DialogTrigger asChild disabled={disabled}>
          <Button>{label}</Button>
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
            <DialogDescription>{description}</DialogDescription>
          </DialogHeader>
          {children}
          {showButtons && (
            <DialogFooter>
              <DialogClose asChild>
                <Button variant="outline">Cancel</Button>
              </DialogClose>
              <Button type="submit">Save changes</Button>
            </DialogFooter>
          )}
        </DialogContent>
      </form>
    </Dialog>
  );
}
