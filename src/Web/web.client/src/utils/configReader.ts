

export function readConfigVal(name: string) : string
{
    return String(import.meta.env["VITE_" + name]);
}
