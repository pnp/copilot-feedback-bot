

export function readConfigVal(name: string) : string
{
    return String(process.env[name] ?? import.meta.env["VITE_" + name]);
}
