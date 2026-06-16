import {referenceTree} from '@/lib/source';
import {DocsLayout} from 'fumadocs-ui/layouts/docs';
import {baseOptions} from '@/lib/layout.shared';
import {LucideBook, LucideCodeXml} from "lucide-react";

export default function Layout({children}: LayoutProps<'/reference'>) {
    return (
        <DocsLayout
            {...baseOptions()}
            tree={referenceTree}
            tabs={[
                {
                    title: 'Documentation',
                    description: 'Primary Documentation',
                    url: '/docs',
                    icon: <LucideBook className="size-full stroke-fd-primary" />
                },
                {
                    title: 'Reference',
                    description: 'C# Script Reference',
                    url: '/reference',
                    icon: <LucideCodeXml className="size-full stroke-sky-500" />
                }
            ]}
        >
            {children}
        </DocsLayout>
    );
}
