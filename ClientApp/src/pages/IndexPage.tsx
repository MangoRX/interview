import { useAuth } from "../components/AuthProvider";
import { upVariants } from "../animations";
import { motion } from "framer-motion";
import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Grid,
    Card,
    CardContent,
    Typography,
    Container,
    Box,
} from "@mui/material";
import { useEffect, useState } from "react";

interface MedicationData {
    ndc: string;
    name: string;
    route: string;
    packaging: string;
}

function IndexPage() {
    const auth = useAuth();
    const [meds, setMeds] = useState<MedicationData[]>([]);
    const [meta, setMeta] = useState({
        total: 0,
        num_Oral: 0,
        num_Topical: 0,
        num_Other: 0,
    });

    const fetchData = async () => {
        try {
            const [medsResponse, metaResponse] = await Promise.all([
                fetch("/api/meds/NDCs"),
                fetch("/api/meds/meta"),
            ]);

            const medsData = await medsResponse.json();
            const metaData = await metaResponse.json();

            console.log(medsData, metaData);

            setMeds(medsData);
            setMeta(metaData);
        } catch (error) {
            console.error("Error fetching data:", error);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    return (
        <motion.div
            variants={upVariants}
            initial={"init"}
            animate={"show"}
            exit={"hide"}
            className={"layout"}
        >
            <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
                <Grid container spacing={3} sx={{ mb: 4 }}>
                    <Grid item xs={12} sm={6} md={3}>
                        <motion.div
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                        >
                            <Card
                                sx={{
                                    bgcolor: "primary.light",
                                    color: "white",
                                }}
                            >
                                <CardContent>
                                    <Typography variant="h6">
                                        Total Medications
                                    </Typography>
                                    <Typography variant="h3">
                                        {meta.total}
                                    </Typography>
                                </CardContent>
                            </Card>
                        </motion.div>
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <motion.div
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                        >
                            <Card
                                sx={{
                                    bgcolor: "success.light",
                                    color: "white",
                                }}
                            >
                                <CardContent>
                                    <Typography variant="h6">
                                        Oral Medications
                                    </Typography>
                                    <Typography variant="h3">
                                        {meta.num_Oral}
                                    </Typography>
                                </CardContent>
                            </Card>
                        </motion.div>
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <motion.div
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                        >
                            <Card
                                sx={{
                                    bgcolor: "warning.light",
                                    color: "white",
                                }}
                            >
                                <CardContent>
                                    <Typography variant="h6">
                                        Topical Medications
                                    </Typography>
                                    <Typography variant="h3">
                                        {meta.num_Topical}
                                    </Typography>
                                </CardContent>
                            </Card>
                        </motion.div>
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <motion.div
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                        >
                            <Card
                                sx={{ bgcolor: "info.light", color: "white" }}
                            >
                                <CardContent>
                                    <Typography variant="h6">
                                        Other Routes
                                    </Typography>
                                    <Typography variant="h3">
                                        {meta.num_Other}
                                    </Typography>
                                </CardContent>
                            </Card>
                        </motion.div>
                    </Grid>
                </Grid>

                <TableContainer component={Paper}>
                    <Table sx={{ minWidth: 650 }} aria-label="medication table">
                        <TableHead>
                            <TableRow>
                                <TableCell>NDC</TableCell>
                                <TableCell>Name</TableCell>
                                <TableCell>Route</TableCell>
                                <TableCell>Packaging</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {meds.map((row) => (
                                <TableRow
                                    key={row.ndc}
                                    sx={{
                                        "&:last-child td, &:last-child th": {
                                            border: 0,
                                        },
                                    }}
                                >
                                    <TableCell>{row.ndc}</TableCell>
                                    <TableCell>{row.name}</TableCell>
                                    <TableCell>{row.route}</TableCell>
                                    <TableCell>{row.packaging}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Container>
        </motion.div>
    );
}

export default IndexPage;

