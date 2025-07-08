import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';

interface Props {
  placeholder: string;
  options: {
    value: string;
    label: string;
  }[];
  label?: string;
  onChange: (value: string) => void;
  defaultValue?: string;
  disabled?: boolean;
}

export function BaseSelect(props: Props) {
  const { placeholder, options, label, onChange, defaultValue, disabled } = props;

  return (
    <Select onValueChange={onChange} defaultValue={defaultValue} disabled={disabled}>
      <SelectTrigger>
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent className="min-w-32">
        <SelectGroup>
          {label && <SelectLabel className="capitalize">{label}</SelectLabel>}
          {options.map((opt) => (
            <SelectItem key={opt.value} className="capitalize" value={opt.value}>
              {opt.label}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
