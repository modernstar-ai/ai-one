# Front End

## Style Sheets
Global Style Sheet is in /src/agile-chat-web/src/global.css

## Tool Choice

## lucide-react
Used for icons
Full list available here: https://lucide.dev/icons

## shadcn
Accessible UI Components
https://ui.shadcn.com/docs

### Common components
- [Accordian](https://ui.shadcn.com/docs/components/accordion): useful for left menus
- [Alert](https://ui.shadcn.com/docs/components/alert)
- [AlertDiaglogue](https://ui.shadcn.com/docs/components/alert-dialog)
- [Button](https://ui.shadcn.com/docs/components/button)
- [Card](https://ui.shadcn.com/docs/components/card): A card with header, content and footer
- [Checkbox](https://ui.shadcn.com/docs/components/checkbox)
- [Collabsible](https://ui.shadcn.com/docs/components/collapsible)
- [Combobox](https://ui.shadcn.com/docs/components/combobox)
- [DataTable](https://ui.shadcn.com/docs/components/data-table)
- [Form](https://ui.shadcn.com/docs/components/form)
- [Input](https://ui.shadcn.com/docs/components/input)
- [Label](https://ui.shadcn.com/docs/components/label) :An accessible label associated with no controls
- [NavigationMenu](https://ui.shadcn.com/docs/components/navigation-menu)
- [ScrollArea](https://ui.shadcn.com/docs/components/scroll-area)
- [Select](https://ui.shadcn.com/docs/components/select)
- [Separator](https://ui.shadcn.com/docs/components/separator)
- [Sheet](https://ui.shadcn.com/docs/components/sheet): Used for left popouts
- [Slider](https://ui.shadcn.com/docs/components/slider)
- [Sonner](https://ui.shadcn.com/docs/components/sonner): A popup toast
- [Switch](https://ui.shadcn.com/docs/components/switch)
- [Table](https://ui.shadcn.com/docs/components/table)
- [Tabs](https://ui.shadcn.com/docs/components/tabs)
- [TextArea](https://ui.shadcn.com/docs/components/textarea)
- [Toast](https://ui.shadcn.com/docs/components/toast)
- [Toggle](https://ui.shadcn.com/docs/components/toggle)
- [ToggleGroup](https://ui.shadcn.com/docs/components/toggle-group)

### Adding UI Components
To add new ui components use the shadcn CLI. 
e.g. [to add the button component](https://ui.shadcn.com/docs/components/button)

```
npx shadcn@latest add button
```

### Usage

```
import { Button } from "@/components/ui/button"
```

```
<Button variant="outline">Button</Button>
```



## TailwindCSS

The colors and other design tokens used in the themes and buttonVariants are defined in the Tailwind CSS configuration file 
src/agile-chat-web/tailwind.config.js:

### postcss 
- Tailwind dependency

### autoprefixer
- Tailwind dependency

### Vite 
Build tool and dev server for JS apps
https://vitejs.dev/guide/why.html

### React
https://react.dev/

### Forms

shadcn form doco: https://ui.shadcn.com/docs/components/form

We are using Zod for form validation: https://zod.dev/